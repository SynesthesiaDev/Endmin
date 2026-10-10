# 🐧 Endmin

![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/SynesthesiaDev/Endmin/deploy.yml?branch=main&style=for-the-badge&label=Build&color=33cc33)
![Last commit](https://img.shields.io/github/last-commit/SynesthesiaDev/Endmin?style=for-the-badge&color=blue&label=Last%20Build)

![Target .NET](https://img.shields.io/badge/.NET-10.0-512bd4?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-black?style=for-the-badge)

A tiny self-hosted pull-based deployment manager for Docker containers

The CI never touches the server. Endmin watches GitHub waits for the build to succeed and pulls and swaps the container automatically

### How it works

1. Every `PollingInterval` milliseconds, Endmin asks GitHub for the latest commit SHA on each configured app's branch
2. If the SHA is different from the last deployed one, it checks the commit's GitHub Actions. It waits until they have all completed successfully, so it never tries deploys a half built image
3. It pulls `<DockerRepository>:<sha>`, stops and removes the old container, and starts a new one with the configured ports and data volume
4. It records the deployed SHA in `hashes.synx` and prunes unused images

### Usage

```yaml
# docker-compose.yml
services:
  endmin:
    network_mode: host
    image: ghcr.io/synesthesiadev/endmin:latest
    restart: unless-stopped
    user: root
    volumes:
      - "./data:/app/data"
      - "/var/run/docker.sock:/var/run/docker.sock" #needs access to the socket
```

```bash
docker compose up -d
```

On first start, Endmin creates `data/config.synx` and `data/hashes.synx`. Edit the config with your github api token, then restart

```bash
docker compose restart endmin
```

> **Note:** Endmin needs access to the Docker socket to manage other containers, which is effectively root access to the host. Only run it on machines you control.

### Configuration

`data/config.synx`:

| Field | Description |
| --- | --- |
| `PollingInterval` | How often to check the github api for new build, in milliseconds (default is `60000`) |
| `GithubToken` | GitHub personal access token. Needs read access to the repositories' contents and checks. Private repos need `repo` scope (classic) or fine-grained read access to Contents, Metadata and Checks |
| `Apps` | List of apps to deploy (below) |

Each app:

| Field | Required | Description |
| --- | --- | --- |
| `Name` | yes | Display name, also the key in `hashes.synx` |
| `ContainerName` | yes | Name of the Docker container Endmin manages |
| `DockerRepository` | yes | Image repository, e.g. `ghcr.io/you/app` |
| `GithubUser` | yes | Owner of the repository |
| `GithubRepo` | yes | Repository name |
| `GithubBranch` | yes | Branch to watch, e.g. `main` |
| `InternalPort` | no | Port the app listens on inside the container |
| `ExternalPort` | no | Port to publish on the host |
| `HostDataPath` | no | Host folder to mount into the container |
| `ContainerDataPath` | no | Mount point inside the container (default `/app/data`) |

### Notes and limitations

- Deploys replace the running container, so expect a few seconds of downtime per update.
- There is no automatic health check or rollback yet. If a new build crashes, the container will keep restarting until you fix it and push again.
- Endmin sets the permissions of each `HostDataPath` to `777` so containers running as any user can write to it.
- With a 60 second interval, GitHub API usage is ~60 requests per app per hour, far below the 5,000/hour limit for authenticated requests. If you are adding more app however be mindful of this limit

<img width="480" height="424" alt="1mf10xducfhg1" src="https://github.com/user-attachments/assets/23430879-fea6-4011-849f-eb07daf8c9dd" />
