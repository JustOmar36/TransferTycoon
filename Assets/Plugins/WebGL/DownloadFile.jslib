mergeInto(LibraryManager.library, {
  DownloadString: function(contentPtr, filenamePtr) {
    var content = UTF8ToString(contentPtr);
    var filename = UTF8ToString(filenamePtr);
    
    var blob = new Blob([content], { type: 'application/json' });
    var url = URL.createObjectURL(blob);
    var element = document.createElement('a');
    element.href = url;
    element.download = filename;
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
    URL.revokeObjectURL(url);
  }
});