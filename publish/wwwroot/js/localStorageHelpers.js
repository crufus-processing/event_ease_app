window.getFilteredLocalStorageKeys = function (prefix) {
    return Object.keys(localStorage).filter(key => key.startsWith(prefix));
};

window.getLocalStorageLength = function () {
    return localStorage.length;
};

window.getLocalStorageKey = function (index) {
    return localStorage.key(index);
};
