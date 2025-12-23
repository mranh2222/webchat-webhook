# Use Microsoft's official ASP.NET Framework image
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8

# Set working directory
WORKDIR /inetpub/wwwroot

# Copy all files
COPY . .

# Expose port 80
EXPOSE 80

# Note: Railway will handle the build process
# This Dockerfile is for reference only

