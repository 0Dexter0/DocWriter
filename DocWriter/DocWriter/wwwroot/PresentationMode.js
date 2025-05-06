export function getAnchors() {
    let preview = document.getElementById('preview');
    
    let anchors = preview.getElementsByTagName('a');
    
    return anchors.map(a => a.attributes[0].nodeValue);
}

function adjustAnchors(element, dotNetObject) {
    
    let anchors = element.getElementsByTagName('a');
    
    if (anchors.length === 0) {
        return;
    }

    for (let anchor of anchors) {
        let refValue = anchor.attributes[0].nodeValue;

        if (!refValue.startsWith('/presentation/')) {
            anchor.attributes[0].nodeValue = '/presentation/' + refValue;
            let referred = document.getElementById(refValue.substring(1));
            dotNetObject.invokeMethodAsync("AddAnchor", anchor.attributes[0].nodeValue, referred.innerText);
        }
    }
}

export function previewHandler(dotNetObject) {
    let preview = document.getElementById('preview');

    let observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            adjustAnchors(mutation.target, dotNetObject);
        });
    });

    observer.observe(preview, { childList: true, subtree: true });
}