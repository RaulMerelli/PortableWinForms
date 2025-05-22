'use strict';

$(document).ready(function () {
    // Disabilita click destro di default del browser
    $(document).bind("contextmenu", function (e) {
        if (!$(e.target).is('input')) {
            return false;
        }
    });
});

function launchPostWindowSuccess(htmlContent, propertiesJson) {
    var properties = JSON.parse(propertiesJson);
    var contentDiv = document.createElement("div");
    contentDiv.style = "height:100%; width:100%";
    var id = '<div style="display:none" class="formId">' + properties.id + '</div>';
    var normalizeWidth = '<div style="display:none" class="normalize-width"></div>';
    var normalizeHeight = '<div style="display:none" class="normalize-height"></div>';
    contentDiv.innerHTML = id + normalizeWidth + normalizeHeight + htmlContent;

    var panel = jsPanel.create({
        content: contentDiv,
        css: {
            'btn-close': (!properties.minimize && !properties.maximize && !properties.mdi) ? 'btn-only-close' : 'btn-regular-close'
        },
        panelSize: properties.size.width + ' ' + properties.size.height,
        border: "0px solid transparent",
        headerControls: {
            smallify: 'remove',
            minimize: !properties.minimize ? 'remove' : '',
            maximize: !properties.maximize ? 'remove' : ''
        },
        headerLogo: properties.icon == "" ? '' : '<img src="' + properties.icon + '"></img>',
        dragit: {
            cursor: "default",
            opacity: 1,
            drag: function (panel) {
                eventHandler(properties.identifier, 'Move');
            },
            start: function (panel) {
                eventHandler(properties.identifier, 'MoveBegin');
            },
            stop: function (panel) {
                eventHandler(properties.identifier, 'MoveEnd');
            }
        },
        resizeit: {
            disable: !properties.resizable,
            resize: function (panel) {
                eventHandler(properties.identifier, 'Resize');
            },
            start: function (panel) {
                eventHandler(properties.identifier, 'ResizeBegin');
            },
            stop: function (panel) {
                eventHandler(properties.identifier, 'ResizeEnd');
            }
        },
        position: {
            my: properties.position.my,
            at: properties.position.at
        },
        headerTitle: properties.title,
        onbeforemaximize: function (panel) {
            document.getElementsByClassName('normalize-width')[0].innerHTML = panel.style.width;
            document.getElementsByClassName('normalize-height')[0].innerHTML = panel.style.height;
            return true;
        },
        onbeforeminimize: function (panel) {
            document.getElementsByClassName('normalize-width')[0].innerHTML = panel.style.width;
            document.getElementsByClassName('normalize-height')[0].innerHTML = panel.style.height;
            return true;
        },
        onbeforenormalize: function (panel) {
            panel.resize({
                width: document.getElementsByClassName('normalize-width')[0].innerHTML,
                height: document.getElementsByClassName('normalize-height')[0].innerHTML
            })
            return true;
        },
        callback: function (panel) {
            if (properties.maximize) {
                panel.titlebar.addEventListener('dblclick', function () {
                    if (panel.status !== 'maximized') {
                        panel.maximize();
                    } else {
                        panel.normalize();
                    }
                });
            }
            eventHandler(properties.identifier, 'Load');
        }
    });
    panel.header.style.background = "transparent";
    if (!properties.minimize && !properties.maximize && !properties.mdi) {
        panel.header.style.height = "26px";
        panel.headerbar.style.height = "26px";
        panel.titlebar.style.height = "26px";
        panel.controlbar.style.height = "25px";
    }
    else if (properties.mdi && !properties.resizable) {
        panel.headerbar.style.height = "26px";
        panel.headerlogo.style.marginTop = "1px";
    }
    panel.style.background = "transparent";
    panel.reposition({
        my: properties.position.my,
        at: properties.position.at
    });

    // Estrai gli script dal contenuto HTML
    var scripts = panel.content.getElementsByTagName('script');
    for (var i = 0; i < scripts.length; i++) {
        executeScript(scripts[i].text);
    }
}

function eventHandler(identifier, eventName) {
    var event = {
        identifier: identifier,
        eventName: eventName
    };
    window.chrome.webview.postMessage(JSON.stringify(event));
}

function executeScript(script) {
    try {
        var scriptElement = document.createElement('script');
        scriptElement.text = script;
        document.head.appendChild(scriptElement).parentNode.removeChild(scriptElement);
    }
    catch (ex) {
        console.log(script);
        console.log(ex);
    }
}

function closeCurrentWindow(control) {
    $(control).parent().closest('.jsPanel').remove();
}

