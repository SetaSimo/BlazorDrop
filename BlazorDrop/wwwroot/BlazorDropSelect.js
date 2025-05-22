window.initBlazorDropSelect = (dotnetHelper, elementId, delay) => {
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

window.BlazorDropSelect = {
    registerClickOutsideHandler: function (dotNetHelper, elementId) {
        function onClick(event) {
            const dropdown = document.getElementById(elementId);
            if (!dropdown || dropdown.contains(event.target)) {
                return;
            }

            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }

        document.addEventListener('click', onClick);

        window[`BlazorDropSelect_cleanup_${elementId}`] = () => {
            document.removeEventListener('click', onClick);
        };
    },

    unregisterClickOutsideHandler: function (elementId) {
        const cleanup = window[`BlazorDropSelect_cleanup_${elementId}`];
        if (cleanup) {
            cleanup();
            delete window[`BlazorDropSelect_cleanup_${elementId}`];
        }
    }
};

window.BlazorDropSelect.registerScrollHandler = function (dotNetHelper, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const onScroll = () => {
        const pixelsBeforeThreshold = 50;
            console.log("32131312")
        if (container.scrollTop + container.clientHeight >= container.scrollHeight - pixelsBeforeThreshold) {
            dotNetHelper.invokeMethodAsync('OnScrollToEndAsync');
        }
    };

    container.addEventListener('scroll', onScroll);

    window[`BlazorDropSelect_scroll_cleanup_${containerId}`] = () => {
        container.removeEventListener('scroll', onScroll);
    };
};

window.BlazorDropSelect.unregisterScrollHandler = function (containerId) {
    const cleanup = window[`BlazorDropSelect_scroll_cleanup_${containerId}`];
    if (cleanup) {
        cleanup();
        delete window[`BlazorDropSelect_scroll_cleanup_${containerId}`];
    }
};