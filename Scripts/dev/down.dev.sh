#!/bin/bash

set -e

cd "$(dirname "$0")/docker" || exit

docker-compose --env-file .env -p lor -f docker-compose.dev.yml down