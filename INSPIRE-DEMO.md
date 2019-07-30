# INSPIRE DEMO

## Device access


### kontron (the actual device)

First at all you ned to determine the device IP. The credentials are the
following:

user: kontron
pass: kontron

To interact with the iotedge component ssh to the device

```
ssh kontron@<IP>
```

and use the iotedge cli util

```
# list all deployed modules of this device
iotedge list

# show the logs of a component
iotedge logs -f --tail=100 enrichment
```

### Grafana

Grafana is reachable vi <IP>:3000

user: admin
pass: admin

Currently only one dashboard is setup, it's called `Inspire`


### Crate

The cratedb is reachable via <IP>:4200

The edge device is publishing its data into the `doc.raw` table. The
enrichment will consume this data and write them into the table `raw.opcdata`.
