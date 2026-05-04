[![AGPL-3.0 License](https://img.shields.io/github/license/Pandetthe/Snapflow?color=%230b0&style=flat-square)](https://github.com/Pandetthe/Snapflow/blob/main/LICENSE)
[![Release](https://img.shields.io/github/v/release/Pandetthe/Snapflow?style=flat-square&color=blue)](https://github.com/Pandetthe/Snapflow/releases)
[![Build & Test](https://img.shields.io/github/actions/workflow/status/Pandetthe/Snapflow/build-and-test.yml?label=Build%20%26%20Test&style=flat-square&logo=githubactions)](https://github.com/Pandetthe/Snapflow/actions/workflows/build-and-test.yml)
[![Security Scan](https://img.shields.io/github/actions/workflow/status/Pandetthe/Snapflow/security.yml?label=Security%20Scan&style=flat-square&logo=githubactions)](https://github.com/Pandetthe/Snapflow/actions/workflows/security.yml)

This project is still under restructuring. The web client is currently not functioning properly due to changes in the API server.
If you are looking for a working version of the application, please check the [proof-of-concept branch](https://github.com/Pandetthe/Snapflow/tree/proof-of-concept).
Alternatively, if you are interested in tracking progress towards the MVP, please refer to this [roadmap](https://github.com/users/Pandetthe/projects/2).

## Unified Deployment

This project includes automated setup scripts to spin up the entire environment using Docker Compose.

### Prerequisites
* Docker and Docker Compose installed and running.
* On Windows, ensure Docker Desktop is active.

### How to Run
**Windows (PowerShell):**
Open your terminal and run:
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup.ps1
```

**Linux / macOS (Bash):**
Open your terminal and run:

```Bash
chmod +x scripts/setup.sh
./scripts/setup.sh
```

Management
View Logs: docker-compose -f deployment/docker-compose.yml logs -f

Stop Services: docker-compose -f deployment/docker-compose.yml down
