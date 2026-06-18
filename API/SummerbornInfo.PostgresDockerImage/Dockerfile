FROM ghcr.io/pgmq/pg18-pgmq:v1.10.0

# Bump this version whenever image-affecting Dockerfile changes are made.
LABEL org.opencontainers.image.version="1.0.0"

USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-18-postgis-3 postgresql-18-postgis-3-scripts \
    && rm -rf /var/lib/apt/lists/*
