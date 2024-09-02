<p align="center">
    <img src="./images/nanuq-logo.jpg" width="25%"  />
</p>


# Nanuq
[![Services](https://github.com/waelouf/Nanuq/actions/workflows/dotnet.yml/badge.svg)](https://github.com/waelouf/Nanuq/actions/workflows/dotnet.yml)
[![Frontend](https://github.com/waelouf/Nanuq/actions/workflows/vue-app.yml/badge.svg)](https://github.com/waelouf/Nanuq/actions/workflows/vue-app.yml)

## About

Nanuq is an open-source application designed to simplify the management of Kafka, Redis, and RabbitMQ for developers. Instead of relying on command-line interfaces for each server, Nanuq provides a unified, user-friendly interface that streamlines daily tasks. Whether you're monitoring queues, managing datasets, or configuring clusters, Nanuq empowers you to handle these critical operations with ease, all from a single UI.

Built with a focus on usability and efficiency, Nanuq aims to reduce the overhead associated with managing these powerful technologies, allowing developers to focus on building and deploying applications rather than managing infrastructure.

## Feature list

### Kafka

- Display server's topics
- Display how many items in each topic
- Add topic

### Redis

- Display Server's details
- Display databases
- Display all string cached keys
- Add item to cache
- Invalidate cache

### RabbitMQ

## Installation

### Pull, build and run

### Docker compose

### Kubernetes deploy

## Disclaimer

Nanuq is currently intended for use in local development and DEV environments only. While we strive to provide a reliable tool for managing Kafka, Redis, and RabbitMQ, this application is not yet ready for production environments. Users should exercise caution and thoroughly test Nanuq in non-production settings before considering any production use. We are continuously working on improvements and welcome contributions from the community to help make Nanuq production-ready in the future.


---

## To-do
- ✅ Store connection in Sqlite database
- All-in-one docker compose
- k8s deploy 
- Connect to the following
  - Kafka
    - Real time
  - RabbitMQ
    - Topic
    - Exchange
    - How many item in each topic
  - Redis
    - Databases
    - Sizes
    - Invalidate?
- Built-in
  - Logs
  - ELK stack
  - OpenTelemetry
- ✅ Aspire
