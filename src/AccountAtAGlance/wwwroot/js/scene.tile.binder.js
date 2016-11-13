//Handles loading jQuery templates dynamically from server
//and rendering them based upon tile data
var tileBinder = function () {
    var templateBase = '/Templates/',

    bind = function (tileDiv, data, renderer) {
        var tileName = tileDiv.attr('id') + 'Template';
        loadTemplate(tileName, function (templates) {
            var acctTemplates = [
                tmpl(tileName + '_Small', data),
                tmpl(tileName + '_Medium', data),
                tmpl(tileName + '_Large', data)
            ];
            tileDiv.data().templates = acctTemplates;
            tileDiv.data().tileData = data;
            renderer(tileDiv);        
        });
    },

    loadTemplate = function (tileName, callback) {
        $.get(templateBase + tileName + '.html', function (templates) {
            $('body').append(templates);
            //See if a partial template is included and register with handlebars
            var partialID = tileName + '_Partial';
            var partial = $('#' + partialID);
            if (partial.length) {
                Handlebars.registerPartial(partialID, partial.html());
            }
            if (callback) callback(templates);
        });
    },

    tmpl = function (tileName, data) {
        //Handlebars functionality to generate HTML from template
        var templateHtml = $('#' + tileName).html();
        if (data) {            
            var compiledTemplate = Handlebars.compile(templateHtml);
            return compiledTemplate(data);
        }
        else {
            return templateHtml;
        }
    };

    return {
        bind: bind,
        loadTemplate: loadTemplate,
        tmpl: tmpl
    };
} ();
