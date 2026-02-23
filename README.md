# Vertical Farm: A Unified Testbed for Human-Agent Collaboration

### Overview

*Vertical Farm* is a cooperative farming simulator designed to address three core challenges in human-agent teaming (HAT) testbed design: expressiveness (supporting diverse research agendas), unification (enabling standardized benchmarks), and rich team dynamics (capturing the complexity of real-world collaboration). Central to the platform is the **Hierarchical Puppetry Interface (HPI)**, which enables dynamic, runtime reconfiguration of control, authority, and team hierarchy by allowing humans and agents to flexibly assume, share, delegate, or relinquish control over task-executing entities. Complementing HPI, Vertical Farm is built around **modular components and standardized abstractions** that balance expressiveness with unification, supporting diverse experimental designs while enabling replicable evaluation and cross-study comparison. By embedding complementary human and agent capabilities directly into both task structure and control hierarchy, *Vertical Farm* serves as a general-purpose testbed for probing when, how, and why hybrid teams succeed (or fail).

### Getting Started

Follow the steps below for a quick setup. For more in-depth description of each of the components of the platform, or setup guide for agent training, please refer to the documentation in the following sections.

1. Clone the git repository

   ```git clone https://github.com/hmt-games/HMT-Game-4.git```

   It should have the following file structure:

   ```
   Vertical Farm
   ├── Assets
   ├── Nakama
   ├── Packages
   └── ProjectSettings
   ```

   The ```Nakama``` folder contains the Nakama server files, and the rest are Unity project files.

2. Setup Nakama networking

   *Vertical Farm* uses Nakama to implement its netwokring features, including online multiplayer and custom matchmaking. The Nakama server lives in a docker container, which can be ran locally or deployed on a remote server.

   To run the server locally, first make sure ```docker-compose``` is installed, then change the terminal directory to ```Nakama``` and run ```docker-compose up```. The ```.yml``` file inside the folder is used to configure the server automatically.

3. Run the game

### Core Gameplay Loop

*Vertical Farm* is a cooperative, multi-floor farming simulation where humans and AI agents jointly manage a **dynamic, partially observable, and stochastic** environment. Teams cultivate plants across stacked farm floors by planting, watering, spraying nutrients, harvesting, and picking fruit, with success typically measured as **yield or throughput under time constraints**. Local actions can produce delayed, system-wide effects, requiring coordinated planning and adaptation.

Humans and agents act through **bots** that operate in specialized modes (e.g., planting, spraying, picking). These modes constrain available actions and must be switched at fixed stations, creating role specialization and interdependence. The gameplay loop can be configured to emphasize different teaming phases, from **exploration and shared sensemaking** to **planning, execution, and dynamic reaction**. Through the **Hierarchical Puppetry Interface (HPI)**, control and hierarchy over bots can shift at runtime, enabling the study of mixed-initiative control, delegation, and evolving team structure within a single task episode.

### Platform Configurations

[Game State Configurations](Documentation/GameStateConfigurations.md)

### Agent Training

Coming soon.



