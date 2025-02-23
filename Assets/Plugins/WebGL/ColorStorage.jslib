var ColorStorage = {
    setColor: function(color) {
        localStorage.setItem("color", color);
        document.querySelector("link[rel='shortcut icon']").setAttribute("href", favicon[color]);
    }
};

mergeInto(LibraryManager.library, ColorStorage);
