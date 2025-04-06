import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

export async function Initialize() {
    mermaid.initialize({ startOnLoad: true });
    await mermaid.run();
}

export function updateFullScreenState(dotNetObject) {
    let container = document.getElementsByClassName('editor-toolbar')[0];

    const observer = new MutationObserver((mutations) => {
        mutations.forEach( (mutation) => {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {

                if (container.className === 'editor-toolbar fullscreen') {
                    container.style.background = 'inherit';
                    dotNetObject.invokeMethodAsync('ToggleFullScreen');
                }
                else if (container.className === 'editor-toolbar') {
                    dotNetObject.invokeMethodAsync('ToggleFullScreen');
                }
            }
        });
    });

    observer.observe(container, {
        attributes: true,
        attributeFilter: ['class'],
    });
}