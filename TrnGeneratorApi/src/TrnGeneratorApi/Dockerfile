# syntax=docker/dockerfile:1
  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  ARG GIT_SHA
  ENV GitSha ${GIT_SHA}
  COPY bin/Release/net8.0/publish/ App/
  WORKDIR /App
  ENTRYPOINT ["dotnet", "TrnGeneratorApi.dll"]
  EXPOSE 80
