#!/bin/bash

set -e

cd docker/ || exit

docker-compose --env-file .env -p lor -f docker-compose.dev.yml down