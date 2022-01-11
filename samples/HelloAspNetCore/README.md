# HelloAspNetCore
This project demostrates how to use RabbitRPC with Project Tye and ASP.NET Core.

The application uses a containerized RabbitMQ instance, so there's no need to setup RabbitMQ, but you'll have to install [docker](https://docs.docker.com/install/) instead.

To run the application locally, you'll also need to install [tye](https://github.com/dotnet/tye/blob/main/docs/getting_started.md).

After setting up the environment, simply execute `tye run` in directory of this file and the application should be up and running shortly.

To explore other topics on Project Tye, such as configuring docker containers or deploying to Kubernetes,
please refer its documentation [here](https://github.com/dotnet/tye/tree/main/docs).