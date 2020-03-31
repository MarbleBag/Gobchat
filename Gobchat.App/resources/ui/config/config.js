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

    //deprecated
    ConfigHelper.setupListener = function (configKey, callback, options) {
        let defOptions = { defValue: undefined }
        defOptions = $.extend(defOptions, options)

        gobconfig.addProfileEventListener((event) => {
            if (event.type === "active")
                callback(gobconfig.get(configKey, defOptions.defValue))
        })
        gobconfig.addPropertyEventListener(configKey, (event) => {
            if (event.isActive)
                callback(gobconfig.get(configKey, defOptions.defValue))
        })
    }

    //deprecated
    ConfigHelper.setupListenerAndInitialize = function (configKey, callback, options) {
        let defOptions = { defValue: undefined }
        defOptions = $.extend(defOptions, options)

        ConfigHelper.setupListener(configKey, callback, options)
        callback(gobconfig.get(configKey, defOptions.defValue))
    }

    //deprecated
    ConfigHelper.linkDropdownConfig = function (element, options) {
        let defOptions = { defValue: undefined }
        defOptions = $.extend(defOptions, options)

        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null) return

        ConfigHelper.setupListenerAndInitialize(configKey, value => element.val(value))

        element.on("change", (event) => {
            const newValue = event.target.value || defOptions.defValue
            gobconfig.set(configKey, newValue)
        })
    }

    //deprecated
    ConfigHelper.linkCheckboxConfig = function (element) {
        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null) {
            element.attr("disabled", true)
            return
        }

        ConfigHelper.setupListenerAndInitialize(configKey, value => element.prop('checked', value))
        element.on("change", event => gobconfig.set(configKey, event.target.checked))
    }

    //deprecated
    ConfigHelper.linkCheckboxValueConfig = function (element, checkValue, uncheckValue) {
        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null) {
            element.attr("disabled", true)
            return
        }

        ConfigHelper.setupListenerAndInitialize(configKey, configValue => element.prop('checked', configValue === checkValue))
        element.on("change", event => gobconfig.set(configKey, event.target.checked ? checkValue : uncheckValue))
    }

    //deprecated
    ConfigHelper.linkCheckboxArrayConfig = function (element, values) {
        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null || values.length == 0) {
            element.attr("disabled", true)
            return
        }

        ConfigHelper.setupListenerAndInitialize(configKey, (configValues) => {
            const checked = _.every(values, (e) => _.includes(configValues, e))
            element.prop('checked', checked)
        })
        element.on("change", (event) => {
            const data = window.gobconfig.get(configKey)
            if (setValuesInArray(data, values, event.target.checked))
                window.gobconfig.set(configKey, data)
        })
    }

    //deprecated
    ConfigHelper.linkTextConfig = function (element) {
        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null) return

        ConfigHelper.setupListenerAndInitialize(configKey, value => element.val(value))

        element.on("change", (event) => {
            gobconfig.set(configKey, event.target.value)
        })
    }

    //deprecated
    ConfigHelper.linkTextCollectionConfig = function (element, options) {
        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null) {
            element.val("")
            return
        }

        let defOptions = { joinSequence: ", " }
        defOptions = $.extend(defOptions, options)

        function cleaner(value) {
            const words = (value || "").split(",")
            return words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
        }

        function joiner(values) {
            return values.join(defOptions.joinSequence)
        }

        ConfigHelper.setupListenerAndInitialize(configKey, values => element.val(joiner(values)))

        element.on("change", function (event) {
            const values = cleaner(event.target.value)
            gobconfig.set(configKey, values)
            element.val(joiner(values))
        })
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

    //deprecated
    ConfigHelper.makeAndBindColorSelector = function (element, options) {
        ConfigHelper.makeColorSelector(element, options)

        const configKey = element.attr(ConfigHelper.ConfigKeyAttribute)
        ConfigHelper.setupListenerAndInitialize(configKey, value => element.spectrum("set", value))
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

    function setValuesInArray(array, values, available) {
        let changed = false

        if (available) {
            values.forEach((value) => {
                if (!_.includes(array, value)) {
                    array.push(value)
                    changed = true
                }
            })
        } else {
            const removedElements =
                _.remove(array, (arrayValue) => {
                    return _.includes(values, arrayValue)
                })

            changed = removedElements.length > 0
        }

        return changed
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