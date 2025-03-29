import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

export async function Initialize() {
    mermaid.initialize({ startOnLoad: true });
    await mermaid.run();
}

export async function RenderAll() {
    await mermaid.run(); 
} 
// export function Render(componentId, definition) {
//     var elements = document.getElementsByClassName(componentId);
//     let index = 1;
//     for(const element of elements)
//     {
//         mermaid.render(`id-${index++}`, element.innerText)
//             .then(result => element.innerHTML = result.svg)
//     }
// }

export function updateState(dotNetObject) {
    let container = document.getElementsByClassName('CodeMirror')[0];

    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                console.log('Button class updated:', container.className);
                dotNetObject.invokeMethodAsync('StateHasChangedFromJs');
            }
        });
    });

    observer.observe(container, {
        attributes: true,
        attributeFilter: ['class'],
    });
}