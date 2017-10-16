$messagingHost		= 'localhost';
$userName			= 'guest';
$password			= 'guest';
$vhost				= '%2f';
$credentials = "$($userName):$($password)";

# Create exchanges - if they exist with same properties will do nothing
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"auto_delete\": false, \"durable\": true, \"type\": \"fanout\" }' -X PUT http://$messagingHost`:15672/api/exchanges/$vhost/test.exchange.1;
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"auto_delete\": false, \"durable\": true, \"type\": \"topic\" }' -X PUT http://$messagingHost`:15672/api/exchanges/$vhost/test.exchange.2;
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"auto_delete\": false, \"durable\": true, \"type\": \"fanout\" }' -X PUT http://$messagingHost`:15672/api/exchanges/$vhost/test.exchange.2.dead;

# Create queues - Static queues only - if they exist with same properties will do nothing
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"auto_delete\": false, \"durable\": true, \"exclusive\": false }' -X PUT http://$messagingHost`:15672/api/queues/$vhost/test.queue.1;
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"auto_delete\": false, \"durable\": true, \"exclusive\": false, \"arguments\": { \"x-message-ttl\": 10000, \"x-dead-letter-exchange\": \"test.exchange.2.dead\" } }' -X PUT http://$messagingHost`:15672/api/queues/$vhost/test.queue.2;
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"auto_delete\": false, \"durable\": true, \"exclusive\": false }' -X PUT http://$messagingHost`:15672/api/queues/$vhost/test.queue.2.dead;

# Create queue bindings - Static queues only - if they exist with same properties will do nothing
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"routing_key\": \"\" }' -X POST http://$messagingHost`:15672/api/bindings/$vhost/e/test.exchange.1/q/test.queue.1;
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"routing_key\": \"test.queue.2\" }' -X POST http://$messagingHost`:15672/api/bindings/$vhost/e/test.exchange.2/q/test.queue.2;
curl.exe -i -u $credentials -H 'Content-Type:application/json' -d '{ \"routing_key\": \"\" }' -X POST http://$messagingHost`:15672/api/bindings/$vhost/e/test.exchange.2.dead/q/test.queue.2.dead;