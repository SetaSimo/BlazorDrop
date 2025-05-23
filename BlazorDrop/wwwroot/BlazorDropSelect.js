window.BlazorDropSelect = (() => {
    const handlers = {};
    function getElementSafe(id) {
        const el = document.getElementById(id);
        if (!el) {
            console.warn(`BlazorDropSelect: Element not found: ${id}`);
        }
        return el;
    }

    function setCleanup(key, fn) {
        handlers[key] = fn;
    }

    function runCleanup(key) {
        const fn = handlers[key];
        if (fn) {
            fn();
            delete handlers[key];
        }
    }

    return {
        initInputHandler(dotnetHelper, elementId, delay) {
            const input = getElementSafe(elementId);
            if (!input) return;

            let debounceTimeout;

            input.addEventListener('input', () => {
                clearTimeout(debounceTimeout);
                debounceTimeout = setTimeout(() => {
                    dotnetHelper.invokeMethodAsync('UpdateSearchListAfterInputAsync');
                }, delay);
            });
        },

        registerClickOutsideHandler(dotNetHelper, elementId) {
            const onClick = (event) => {
                const dropdown = getElementSafe(elementId);
                if (!dropdown || dropdown.contains(event.target)) return;

                dotNetHelper.invokeMethodAsync('CloseDropdownAsync');
            };

            document.addEventListener('click', onClick);
            setCleanup(`click_${elementId}`, () => {
                document.removeEventListener('click', onClick);
            });
        },

        unregisterClickOutsideHandler(elementId) {
            runCleanup(`click_${elementId}`);
        },

        registerScrollHandler(dotNetHelper, containerId) {
            const container = getElementSafe(containerId);
            if (!container) return;

            const onScroll = () => {
                const pixelsBeforeThreshold = 50;
                if (container.scrollTop + container.clientHeight >= container.scrollHeight - pixelsBeforeThreshold) {
                    dotNetHelper.invokeMethodAsync('OnScrollToEndAsync');
                }
            };

            container.addEventListener('scroll', onScroll);
            setCleanup(`scroll_${containerId}`, () => {
                container.removeEventListener('scroll', onScroll);
            });
        },

        unregisterScrollHandler(containerId) {
            runCleanup(`scroll_${containerId}`);
        }
    };
})();
