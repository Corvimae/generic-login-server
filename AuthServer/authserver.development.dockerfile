FROM microsoft/dotnet:2.1-sdk

# Set environment variables
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_ENVIRONMENT="Development"
  
EXPOSE 443/tcp
EXPOSE 80/tcp

# Copy files to app directory
COPY . /app
 
# Set working directory
WORKDIR /app
 
# Restore NuGet packages
RUN ["dotnet", "restore"]


RUN chmod +x entrypoint.sh

CMD /bin/bash entrypoint.sh