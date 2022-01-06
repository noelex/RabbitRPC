using RabbitRPC.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.FileSystem
{
    internal class FileSystemStateTransaction : ITransaction, IStateStore
    {
        private readonly FileStream _dataStream;
        private readonly IsolationLevel _isolationLevel;
        private readonly string _fileName;
        private readonly IMessageBodySerializer _serializer;
        private readonly BinaryReader _reader;
        private readonly Dictionary<string, object?> _pendingUpdates=new Dictionary<string, object?>();
        private readonly List<string> _pendingRemoves = new List<string>();
        private readonly List<IPendingOperation> _pendingOperations = new List<IPendingOperation>();

        private readonly IDisposable? _rangeLock;

        public FileSystemStateTransaction(IsolationLevel isolationLevel, string fileName, FileStream dataStream, IMessageBodySerializer messageBodySerializer, IDisposable? rangeLock)
        {
            _isolationLevel = isolationLevel;
            _fileName = fileName;
            _dataStream = dataStream;
            _reader = new BinaryReader(_dataStream, Encoding.UTF8, false);
            _serializer = messageBodySerializer;
            _rangeLock = rangeLock;
        }

        public static async Task<FileSystemStateTransaction> CreateAsync(IsolationLevel isolationLevel,
            string fileName,IMessageBodySerializer messageBodySerializer,CancellationToken cancellationToken)
        {
            if (isolationLevel == IsolationLevel.Default)
            {
                isolationLevel = IsolationLevel.ReadCommitted;
            }

            if (isolationLevel != IsolationLevel.ReadCommitted && isolationLevel != IsolationLevel.Serializable)
            {
                throw new NotSupportedException("File system state transaction supports only ReadCommitted and Serializable isolation level.");
            }

            var dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var fs= new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            IDisposable? rangeLock = null;
            if (isolationLevel == IsolationLevel.Serializable)
            {
                rangeLock =  await LockRangeAsync(fileName, fs, 0, fs.Length, cancellationToken);
            }

            return new FileSystemStateTransaction(isolationLevel, fileName, fs, messageBodySerializer, rangeLock);
        }

        private async static Task<IDisposable> LockRangeAsync(string fileName, FileStream dataStream, long pos, long length, CancellationToken cancellationToken=default)
        {
            var lockFile = fileName + ".lock";
            var tcs = new TaskCompletionSource<bool>();
            using var watcher = new FileSystemWatcher(Path.GetDirectoryName(fileName), Path.GetFileName(lockFile));
            watcher.Created += (s, e) => tcs = new TaskCompletionSource<bool>();
            watcher.Deleted += (s, e) => tcs.TrySetResult(true);
            watcher.EnableRaisingEvents = true;

            using var _ = cancellationToken.Register(() => tcs.TrySetCanceled());

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    dataStream.Lock(pos, length);
                    var lockFs = new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, 1, FileOptions.DeleteOnClose);
                    return new ActionDisposable(() =>
                    {
                        dataStream.Unlock(pos, length);
                        lockFs.Dispose();
                    });
                }
                catch (IOException)
                {
                    await tcs.Task;
                    continue;
                }
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            var local = _rangeLock == null;
            IDisposable? rlock = null;
            if(local) rlock = await LockRangeAsync(_fileName, _dataStream, 0, _dataStream.Length, cancellationToken);

            try
            {
                var snapshot = ReadAll(_dataStream);
                foreach (var pop in _pendingOperations)
                {
                    pop.Execute(snapshot);
                }

                _dataStream.SetLength(0);
                using var writer = new BinaryWriter(_dataStream, Encoding.UTF8, true);
                foreach (var (k, v) in snapshot)
                {
                    writer.Write(k);
                    var buffer = _serializer.SerializeWithTypeInfo(v);
                    writer.Write(buffer.Length);
                    writer.Write(buffer.Span);
                }

                writer.Flush();
            }
            finally
            {
                rlock?.Dispose();
            }
        }

        private Dictionary<string, object?> ReadAll(FileStream stream)
        {
            var result = new Dictionary<string, object?>();

            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var key= _reader.ReadString();
                var sz = _reader.ReadInt32();
                var value = _reader.ReadBytes(sz);
                result[key] = _serializer.DeserializeWithTypeInfo(value);
            }

            return result;
        }

        public bool TryFindKey(string key)
        {
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var k = _reader.ReadString();
                var sz = _reader.ReadInt32();

                if (k == key)
                {
                    return true;
                }
                _reader.BaseStream.Position += sz;
            }

            return false;
        }

        public void Dispose()
        {
            _rangeLock?.Dispose();
            _reader.Dispose();
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public bool TryGetValue(string key, out object? value)
        {
            if (_pendingRemoves.Contains(key))
            {
                value = null;
                return false;
            }

            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var k = _reader.ReadString();
                var sz = _reader.ReadInt32();

                if (k == key)
                {
                    var val = _reader.ReadBytes(sz);
                    value = _serializer.DeserializeWithTypeInfo(val);

                    return true;
                }
                _reader.BaseStream.Position += sz;
            }

            value = null;
            return false;
        }

        public void Put(string key, object? value)
        {
            _pendingUpdates[key] = value;
            _pendingOperations.Add(new PendingPut(key, value));
        }

        public bool Remove(string key)
        {
            if (!_pendingRemoves.Contains(key) && TryFindKey(key))
            {
                _pendingRemoves.Add(key);
                _pendingOperations.Add(new PendingRemove(key));
                return true;
            }

            return false;
        }
    }
}
