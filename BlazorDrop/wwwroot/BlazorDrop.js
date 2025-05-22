window.initLazySelect = (dotnetHelper, elementId, delay) => {
    let debounceTimeout;
    const input = document.getElementById(elementId);

    if (!input) return;

    input.addEventListener('input', () => {
        clearTimeout(debounceTimeout);

        debounceTimeout = setTimeout(() => {
            dotnetHelper.invokeMethodAsync('UpdateSearchListAfterInputAsync');
        }, delay);
    });
};

window.lazyLoadSelect = {
    registerClickOutsideHandler: function (dotNetHelper, elementId) {
        function onClick(event) {
            const dropdown = document.getElementById(elementId);
            if (!dropdown || dropdown.contains(event.target)) {
                return;
            }

            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }

        document.addEventListener('click', onClick);

        window[`lazyLoadSelect_cleanup_${elementId}`] = () => {
            document.removeEventListener('click', onClick);
        };
    },

    unregisterClickOutsideHandler: function (elementId) {
        const cleanup = window[`lazyLoadSelect_cleanup_${elementId}`];
        if (cleanup) {
            cleanup();
            delete window[`lazyLoadSelect_cleanup_${elementId}`];
        }
    }
};
