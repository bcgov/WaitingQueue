# Kong Error Handling

This provides a ui component that can be used in a Kong `post-function` to render
a error page with a next action.

## Examples

```html
<!doctype html>
<html>
<head>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link
        href="https://fonts.googleapis.com/css2?family=Noto+Sans:wght@200;700&display=swap"
        rel="stylesheet"
    />
    <script src="https://bcstats-dev.apps.gov.bc.ca/errors.js?v=1.0.0"></script>
</head>
</body>
    <error-display message="Expired Session" action="back"/>
</body>
</html>
```

Alternates:

```html
<error-display 
    message="Request Timed Out"
    detail="Survey system took too long to complete request.  Please click the back button and try submitting again"
    action="back"/>
```
