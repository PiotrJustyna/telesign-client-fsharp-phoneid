# readme

Basic f# project illustrating how to use the telesign api's phoneid with hmac authentication.

At the moment of writing this, the documentation:

* https://developer.telesign.com/enterprise/reference/createpidrequest
* https://developer.telesign.com/enterprise/docs/authentication

is not terribly precise and does not cover f#. Furthermore, the c# code examples provided are rather old, convoluted, not very portable and not bundled as nuget packages. This code does not rely on state in contrast to the c# examples, which makes it substantially more portable and, hopefully, easier to use and reason about.