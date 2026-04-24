# RaceService
RaceService for events info and past results

# Setup Local Ollama Mode with Continue Dev

To use your local LLM with Continue Dev, follow these steps:

## Step 1: Start the Ollama Server
First, start the Ollama server on your local machine.

```sh
OLLAMA_HOST=0.0.0.0 ollama serve
```

The Ollama server will be hosted at `localhost:11434`.

You can also list all available models using:

```sh
ollama list
```

or 
```sh
curl http://localhost:11434/api/tags
```

To run the Ollama model, for beginners use `qwen2.5-coder:7b`:

```sh
ollama run qwen2.5-coder:7b
```

## Step 2: Update Continue Dev Configuration

Next, update the `config.yaml` file in Continue Dev to use your local Ollama model.

- name: Qwen Local
  provider: ollama
  model: qwen2.5-coder:7b
  roles:
    - chat
    - edit
    - apply

## Step 3: Run Open-WebUI for a ChatGPT-like UI

To set up a ChatGPT-like UI on your browser, use the open-webui Docker setup.

1. **Create and run a new Docker container at port 3000 redirected to 8080:**

    ```sh
    docker run -d \
      -p 3000:8080 \
      -v open-webui:/app/backend/data \
      --name open-webui \
      ghcr.io/open-webui/open-webui:main     
    ```

2. **Remove the image and view stats of running containers:**

    ```sh
    docker rm -f open-webui  
    docker stats 
    ```

## Step 4: View Running Containers and Logs

To check which Docker containers are currently running on your machine:

```sh
docker ps 
```

To view logs specific to the `open-webui` running container:

```sh
docker logs open-webui  
```

To Run the project
```sh
dotnet run --project /home/balagod99/Dev/RaceService/RaceService/RaceService.Api/RaceService.Api.csproj
```


To make new migartion run from root RaceService:

```sh
dotnet ef migrations add 25042026_addedRaceEntryDrivers   --project RaceService.Application   --startup-project RaceService.Api
```