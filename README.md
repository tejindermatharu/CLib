# CacheLib

Please run the CacheTest project tests to Test and use the Custom cache.

Some comments:

 - Have experimnented with a underlying Dictionary (with more syncronisation) in one implementation, but also with a ConcurrentDictioanry (less syncronisation) and did some simple benchmarking. One with Dictionary and more locking seemsed faster, but more testing and consistency checking would be needed with more time.
 - Have used another singleton using mediator pattern (with events) for creating a Notifier for expelled cache items. Have used this rather than using a simple callback, as a callback would be as short lived as the calling method. The mediator can be used anywhere in you app.
 
 With more time I would:
 
 - More testing for consistency and edge case scenarios in both single threaded and muti-threaded scenarios as well as more benchmarking.
 - More general methods on the cache class i.e. removeItem, as well as more options for setting expiry and the management of exipired cache items.
 - Removed items Notifications can be even more robust and scalable with a service bus implementation, which can allow for inter-app/service comminucations and alerts.
