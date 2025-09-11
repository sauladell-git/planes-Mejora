begin transaction;

SET IDENTITY_INSERT Fields ON;

update TemplateTypeFields set Field = '[COD_EJE]', Description = REPLACE(Description, 'campo', 'eje') where Field = '[COD_CAMPO]';
update TemplateTypeFields set Field = '[EJE]', Description = REPLACE(Description, 'Campo', 'Eje') where Field = '[CAMPO]';
update Templates set Content = REPLACE(Content, '[COD_CAMPO]', '[COD_EJE]') where Content like '%[COD_CAMPO]%';
update Templates set Content = REPLACE(Content, '[CAMPO]', '[EJE]') where Content like '%[CAMPO]%';

insert into Fields (Id, Description, Management, Code) VALUES (11,'Fortalecimiento de la trayectoria','INET','I'), 
(12,'Vinculación con los sectores científico - tecnológico y socio – productivo','INET','II'), 
(13,'Desarrollo profesional docente','INET','III'), 
(14,'Mejora de entornos formativos','INET','IV');

insert into Lines (FieldId, Code, Description) VALUES ('11','A','Participación en Encuentros Educativos de la ETP'),
('11','B','Apoyo y acompañamiento de los procesos de aprendizaje'),
('11','C','Acciones para favorecer el completamiento de carreras técnicas de nivel secundario en particular para aquellos que no han cumplimentado todos los requisitos para la graduación'),
('11','D','Mochila técnica'),
('11','E','Traslado para estudiantes'),
('11','F','Equipamiento de albergues, gimnasios y comedores estudiantiles'),
('11','G','Estrategias para la promoción de la igualdad de género'),
('11','H','Acciones para inclusión para personas con discapacidad'),
('11','I','Acciones para estudiantes en situación de encierro'),
('12','A','Acciones para favorecer la realización de Prácticas Profesionalizantes'),
('12','B','Acciones que involucran a los sectores científico tecnológico y socio productivo'),
('12','C','Visitas didácticas y viajes de estudio vinculados con las orientaciones y/o especialidades de las trayectorias formativas de la ETP'),
('12','D','Acciones tendientes a la protección y sostenibilidad ambiental'),
('13','A','Formación Docente Inicial'),
('13','B','Formación docente Continua'),
('13','C','Formación de Instructores'),
('13','D','Acciones para el desarrollo profesional para directivos, docentes, inspectores, supervisores y equipos técnicos provinciales'),
('13','E','Autoevaluación Institucional'),
('14','A','Equipamiento, materiales e insumos para el desarrollo de actividades y uso seguro del entorno formativo en talleres, laboratorios, espacios productivos y deportivos'),
('14','B','Acciones para el equipamiento integral de las bibliotecas de las instituciones de ETP'),
('14','C','Tecnologías de la Información y la Comunicación'),
('14','D','Funcionamiento de Aulas Talleres Móviles.'),
('14','E','Infraestructura física educativa de las instituciones de Educación Técnico Profesional, de acuerdo a lo establecido por la RCF N° 279/16 y en forma complementaria por la presente');

SET IDENTITY_INSERT Fields OFF;

COMMIT;

