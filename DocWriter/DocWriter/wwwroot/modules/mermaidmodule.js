import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

export async function Initialize() {
    // mermaid.initialize({ startOnLoad: true, theme: 'dark' });
    mermaid.initialize({ startOnLoad: true, darkMode: true, theme: 'dark' });
    await mermaid.run();
}

export async function RenderAll() {
    await mermaid.run();
}

export function editorStateChangingHandler(dotNetObject) {
    let toolbars = document.getElementsByClassName('editor-toolbar');
    
    if (toolbars.length > 0) {
        let toolbar = toolbars[0];
        let editorPreview = document.getElementsByClassName('editor-preview-side')[0];

        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {

                    if (mutation.target.isSameNode(toolbar)) {
                        if (toolbar.className === 'editor-toolbar fullscreen') {
                            toolbar.style.background = 'inherit';
                            dotNetObject.invokeMethodAsync('ToggleFullScreen');
                        } else if (toolbar.className === 'editor-toolbar') {
                            dotNetObject.invokeMethodAsync('ToggleFullScreen');
                        }
                    } else if (mutation.target.isSameNode(editorPreview)) {
                        if (editorPreview.classList.contains('editor-preview-active-side')) {
                            editorPreview.style.color = '#000';
                        }

                        dotNetObject.invokeMethodAsync('StateHasChangedFromJs');
                    }

                }
            });
        });

        observer.observe(toolbar, {
            attributes: true,
            attributeFilter: ['class'],
        });
        observer.observe(editorPreview, {
            attributes: true,
            attributeFilter: ['class'],
        });
    }
}