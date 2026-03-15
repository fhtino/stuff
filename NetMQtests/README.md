# NetMQ tests

## PPClient + PPServer

A simple client/server demo using NetMQ's `PushSocket` and `PullSocket`. Optional encryption can be enabled to secure the communication.

Note: Currently NetMQ does not provide a way to inspect the public key presented by a client. I would like to be able to identify the calling client and allow or deny connections based on its public key. Any ideas?



