'use strict';

var ConfigHelper = (function (ConfigHelper, undefined) {
    ConfigHelper.ConfigKeyAttribute = "data-gob-configkey"

    ConfigHelper.generateId = function (length, ids) {
        let id = null
        do {
            id = Gobchat.generateId(length)
        } while (_.includes(ids, id))
        return id
    }

    ConfigHelper.showProfileIdSelectionDialog = function (callback, options) {
        let defOptions = { exclude: undefined }
        defOptions = $.extend(defOptions, options)

        const dialog = $('#config_copyprofiledialog')
        const dialogSelection = dialog.find("select")

        dialogSelection.empty()
        let profiles = gobconfig.profiles
        if (defOptions.exclude)
            profiles = _.without(gobconfig.profiles, defOptions.exclude)

        profiles.forEach((profileId) => {
            var profile = gobconfig.getProfile(profileId)
            dialogSelection.append(new Option(profile.profileName, profileId))
        })

        dialog.show()
        dialog.dialog({
            modal: true,
            buttons: {
                "Ok": function () {
                    const selectedProfileId = dialogSelection.val()
                    callback(selectedProfileId)
                    $(this).dialog("close");
                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            }
        })
    }

    ConfigHelper.showErrorDialog = function () {
        //TODO
    }

    ConfigHelper.makeColorSelector = function (element, options) {
        const defaultOptions = {
            hasAlpha: true,
            hasReset: true,
            onBeforeShow: null
        }

        options = $.extend(defaultOptions, options)

        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null)
            throw new Error(`Attribute '${ConfigHelper.ConfigKeyAttribute}' not set`)

        element.spectrum({
            preferredFormat: "hex3",
            // color: data,
            allowEmpty: true,
            showInput: true,
            showInitial: true,
            showAlpha: options.hasAlpha,
            showPalette: true,
            //palette: spectrumColorPalette,
            showSelectionPalette: true,
            selectionPalette: [],
            maxSelectionSize: 6,
            clickoutFiresChange: false,
            hide: function (color) {
                if (color !== null)
                    gobconfig.set(configKey, color.toString())
                else
                    gobconfig.set(configKey, null)
            },
            beforeShow: function (color) {
                if (options.onBeforeShow) {
                    const result = options.onBeforeShow(input)
                    return result === undefined || result === null || Boolean(result)
                }
                return true
            }
        })
    }

    ConfigHelper.getConfigKey = function (element) {
        return $(element).attr(ConfigHelper.ConfigKeyAttribute)
    }

    ConfigHelper.setConfigKey = function (element, configKey) {
        return $(element).attr(ConfigHelper.ConfigKeyAttribute, configKey)
    }

    ConfigHelper.makeResetButton = function (element) {
        $(element).on("click", () => gobconfig.reset(ConfigHelper.getConfigKey(element)))
    }

    ConfigHelper.makeCopyProfileButton = function (element, options) {
        const defOptions = { callback: undefined, configKeys: [] }
        options = $.extend(defOptions, options)

        function copyProfile(profileId) {
            if (options.callback) {
                let result = options.callback()
                if (!(result === undefined || result === null) || !Boolean(result))
                    return
            }

            const srcProfile = gobconfig.getProfile(profileId)
            const dstProfile = gobconfig.getProfile(gobconfig.activeProfile)
            options.configKeys.forEach(key => dstProfile.copyFrom(srcProfile, key))
        }

        element.on("click", event => ConfigHelper.showProfileIdSelectionDialog(copyProfile, { exclude: gobconfig.activeProfile }))

        const checkCopyProfileState = () => element.attr("disabled", (gobconfig.profiles.length <= 1))
        gobconfig.addProfileEventListener(event => checkCopyProfileState())
        checkCopyProfileState()
    }

    ConfigHelper.decodeHotkey = function (keyEvent, ignoreEnter) {
        if (ignoreEnter && event.keyCode == 13) // enter
            return null

        if (event.keyCode === 16 || event.keyCode === 17 || event.keyCode === 18)
            return ""

        keyEvent.preventDefault()

        let msg = ""
        if (keyEvent.shiftKey) msg += "Shift + "
        if (keyEvent.altKey) msg += "Alt + "
        if (keyEvent.ctrlKey) msg += "Ctrl + "

        var keyEnum = Gobchat.KeyCodeToKeyEnum(keyEvent.keyCode)
        if (keyEnum === null) {
            msg = ""
        } else {
            msg += keyEnum
        }
        return msg
    }

    return ConfigHelper
}(ConfigHelper || {}));