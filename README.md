# LOR
ЛабОчередь - приложение для организации очередей на лабораторные работы для студентов

## Architecture

```mermaid
flowchart TD;
    A[Database Service] -->|Retrieves data| B((gRPC));
    B -->|Retrieves data| D[TelegramApp Service];
    D -->|Sets data| B;
    C[GroupSchedule Service] -->|Sets data| B;
    B --> |Sets data| A
```





