# Proyecto de la asignatura de Ingeniería de Protocolos

Esta es la memoria del proyecto `SERVICIO CONVERSACIONAL MULTIUSUARIO (APL05)` de la asignatura de Ingeniería de Protocolos del año 22-23.

## Autor

- [Nicolás Cossío Miravalles](mailto:<nicocossiom@gmail.com>) - Matrícula: b190082 - [n.cossio@alumnos.upm.es](mailto:<n.cossio@alumnos.upm.es>)

## Documentación del proyecto

> Atención: Se recomienda leer la documentación en el formato web, ya que en el formato PDF no se muestran correctamente las imágenes y diagramas.

Puede encontrar la documentación del proyecto en el siguiente [enlace](https://nicocossiom.github.io/IngenieriaProtocolos/). La documentación está escrita en inglés.

En el enlace encontrará:

- Explicación sobre la problemática del proyecto.
- Explicación de la solución propuesta.
- Manual de uso de las aplicaciones de CLI desarrolladas:
  - [Cliente](https://nicocossiom.github.io/IngenieriaProtocolos/cliente.html)
  - [Servidor](https://nicocossiom.github.io/IngenieriaProtocolos/server.html)

- .NET API Docs. Esto es la documentación interna del el código generada de los comentarios en el código.

## Enunciado del proyecto

DESCRIPCIÓN:

Implementación de un servicio conversacional “chat” multiusuario.

Este servicio estará formado por una entidad central retransmisora y por los distintos usuarios del
sistema. El objetivo es que una vez que los usuarios se hayan registrado a la entidad
retransmisora, ésta se encargue de difundir los datos que le envíe un usuario a todos
los usuarios registrados.

Las características generales del servicio serán las siguientes:

− Cada usuario abrirá dos sockets UDP uno para enviar datos y otro para recibir datos. Los puertos de estos dos sockets deberán ser consecutivos.
− La entidad retransmisora dispondrá de dos sockets en dos puertos distintos, ofreciendo dos servicios: un puerto de registro de usuarios servirá para que los usuarios del sistema se registren. El otro puerto (puerto de difusión de datos) servirá para recibir datos, que serán retransmitidos a todas las entidades registradas.
− Cuando la entidad retransmisora reciba una petición por el puerto de registro de usuarios almacenará la dirección del usuario en una tabla de usuarios.
− Cuando la entidad retransmisora reciba una petición por el puerto de difusión de datos retransmitirá los datos a todos los usuarios registrados. Asimismo, se mandará un asentimiento al usuario

## Licencia

Este proyecto está bajo la licencia MIT. Para más información, ver el archivo [LICENSE](https://github.com/nicocossiom/IngenieriaProtocolos/blob/master/LICENSE)
