#!/bin/bash

set -e

cd "$(dirname "$0")/docker" || exit

docker-compose --env-file .env -f docker-compose.dev.yml up -d --build