page.includeJs('http://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js', function () {
    if (page.injectJs()) {
        var title = page.evaluate(function () {
            // returnTitle is a function loaded from our do.js file - see below
            return returnTitle();
        });
        console.log(title);
        phantom.exit();
    }
});