var tileRenderer = function () {

    var render = function (tileDiv, sceneId) {
        var data = tileDiv.data();
        if (data.templates) {
            if (!sceneId) sceneId = 0;
            var size = data.scenes[sceneId].size,
                formatterFunc = data.formatter;
            tileDiv.html(data.templates[size]);
            if (formatterFunc != null) {
                formatterFunc(tileDiv);
            }
        }
    };

    return {
        render: render
    };

} ();




