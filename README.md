PostgreSql Kodları:
```
CREATE FUNCTION notify_account_changes()
RETURNS trigger AS $$
BEGIN
  PERFORM pg_notify(
    'accounts_changed',
    json_build_object(
      'operation', TG_OP,
      'record', row_to_json(NEW)
    )::text
  );

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

```
CREATE TRIGGER accounts_changed
AFTER INSERT
ON mustafa
FOR EACH ROW
EXECUTE PROCEDURE notify_account_changes()
```
