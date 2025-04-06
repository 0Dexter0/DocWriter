import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

export async function Initialize() {
    mermaid.initialize({ startOnLoad: true });
    await mermaid.run();
}

export async function RenderAll() {
    await mermaid.run();
}

export function editorStateChangingHandler(dotNetObject) {
    let toolbar = document.getElementsByClassName('editor-toolbar')[0];
    let mdeContainer = document.getElementsByClassName('EasyMDEContainer')[0];

    const observer = new MutationObserver((mutations) => {
        mutations.forEach( (mutation) => {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {

                if (mutation.target.isSameNode(toolbar)) {
                    if (toolbar.className === 'editor-toolbar fullscreen') {
                        toolbar.style.background = 'inherit';
                        dotNetObject.invokeMethodAsync('ToggleFullScreen');
                    }
                    else if (toolbar.className === 'editor-toolbar') {
                        dotNetObject.invokeMethodAsync('ToggleFullScreen');
                    }
                }
                else if (mutation.target.isSameNode(mdeContainer)) {
                    dotNetObject.invokeMethodAsync('StateHasChangedFromJs');
                }
                
            }
        });
    });

    observer.observe(toolbar, {
        attributes: true,
        attributeFilter: ['class'],
    });
    observer.observe(mdeContainer, {
        attributes: true,
        attributeFilter: ['class'],
    });
}