SELECT 
  -- Distinct - for cases when there are several stored procedures with same name but different params
  Distinct
  o.OBJECT_NAME, 
  p.PROCEDURE_NAME, 
  a.ARGUMENT_NAME 
FROM 
  ALL_OBJECTS o,
  ALL_PROCEDURES p,
  USER_ARGUMENTS a
WHERE 
  o.OBJECT_TYPE = 'PACKAGE'
  -- Specify oracle schema 
  and o.OWNER = 'HR'
  and p.OWNER = o.OWNER
  and p.OBJECT_NAME = o.OBJECT_NAME
  and p.PROCEDURE_NAME is not null
  and a.PACKAGE_NAME = o.OBJECT_NAME
  and a.OBJECT_NAME = p.PROCEDURE_NAME
  and a.DATA_LEVEL = 0

  -- comment for get all packages or change query as you need
/*  and o.OBJECT_NAME in 
  (
  -- list stored procedures here
  )
*/
order by o.OBJECT_NAME, p.PROCEDURE_NAME