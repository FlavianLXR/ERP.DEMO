function downloadFile(fileName, base64Data, mimeType) {
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.getElementHeight = (id) => {
    let el = $('.'+id);
    console.log(el + el.height());
    return el ? el.height() : 0;
};

window.getDataGridContainerHeight = (fullscreen) => {
    let e1 = $('.mud-toolbar');
    //e1.height() = 76; //2 lignes de toolbar
    let e2 = $('.mud-table-pagination-toolbar');
    let viewport = getViewportHeight();
    let e3;
    if (fullscreen == true) {
        e3 = (((viewport - (76 + e2.height())) * 100 / viewport)-1).toString() + "vh";
    }
    else {
        //e3 = (((viewport - (76 + e1[0].offsetTop + e2.height())) * 100 / viewport)-1).toString() + "vh";
        e3 = "60vh";
    }
    return e3;
};

window.getViewportHeight = () => window.innerHeight;


//var url = $"product/details/{id}";
//await JSRuntime.InvokeVoidAsync("openInNewTab", url);
window.openInNewTab = function (url) {
    window.open(url, '_blank');
};