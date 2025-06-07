window.BlazorDropSelect = (() => {
    const handlers = {};
    function getElementSafe(id) {
        const element = document.getElementById(id);
        if (!element) {
            console.warn(`BlazorDropSelect: Element not found: ${id}`);
        }
        return element;
    }

    function setCleanup(key, fn) {
        handlers[key] = fn;
    }

    function runCleanup(key) {
        console.log(key)
        const fn = handlers[key];
        if (fn) {
            fn();
            delete handlers[key];
        }
    }

    return {
        initInputHandler(dotnetHelper, elementId, delay, clickOutsideSelectorId) {
            const input = getElementSafe(elementId);
            if (!input) return;

            let debounceTimeout;

            input.addEventListener('input', () => {
                clearTimeout(debounceTimeout);
                debounceTimeout = setTimeout(() => {
                    dotnetHelper.invokeMethodAsync('UpdateSearchListAfterInputAsync', clickOutsideSelectorId);
                }, delay);
            });
        },

        registerClickOutsideHandler(dotNetHelper, elementId) {
            const onClick = (event) => {
                const dropdown = getElementSafe(elementId);
                if (!dropdown || dropdown.contains(event.target)) return;
                console.log(elementId)
                dotNetHelper.invokeMethodAsync('OnClickOutsideAsync', elementId);
            };

            document.addEventListener('click', onClick);
            setCleanup(`click_${elementId}`, () => {
                document.removeEventListener('click', onClick);
            });
        },

        unregisterClickOutsideHandler(elementId) {
            runCleanup(`click_${elementId}`);
        },

        registerScrollHandler(dotNetHelper, containerId, methodName, threshold = 50) {
            const container = getElementSafe(containerId);
            if (!container) return;

            const onScroll = () => {

                if (container.scrollTop + container.clientHeight >= container.scrollHeight - threshold) {
                    dotNetHelper.invokeMethodAsync(methodName);
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