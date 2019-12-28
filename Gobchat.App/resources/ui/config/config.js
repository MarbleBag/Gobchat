'use strict';

function initializeNavigationBar(navigationBarId) {
    const navAttribute = "data-nav-target"

    function hideNavigationContent(navigationId) {
        $(`#${navigationId} > a`).each(function () {
            const target = $(this).attr(navAttribute)
            if (target)
                $(`#${target}`).hide()
        })
    }

    function showActiveNavigationContent(navigationId) {
        $(`#${navigationId} > .active`).each(function () {
            const target = $(this).attr(navAttribute)
            if (target)
                $(`#${target}`).show()
        })
    }

    function importNavigationContent(navigationId) {
        $(`#${navigationId} > a`).each(function () {
            const targetId = $(this).attr(navAttribute)
            if (!targetId) return
            const target = $(`#${targetId}`)
            const contentSrc = target.attr("data-content-src")
            if (!contentSrc) return
            target.load(contentSrc)
        })
    }

    function hasNavigationAttribute(element) {
        const target = $(element).attr(navAttribute)
        return target !== null && target !== undefined
    }

    hideNavigationContent(navigationBarId)
    importNavigationContent(navigationBarId)
    showActiveNavigationContent(navigationBarId)

    $(`#${navigationBarId}`).on("click", function (event) {
        if (!hasNavigationAttribute(event.target)) return
        hideNavigationContent(navigationBarId)
        $(`#${navigationBarId} > .active`).removeClass("active")
        $(event.target).addClass("active")
        showActiveNavigationContent(navigationBarId)
    })
}

initializeNavigationBar("main_navbar")