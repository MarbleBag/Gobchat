/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

'use strict';

(async function () {
    const ConfigKeyData = "behaviour.mentions.data"
    const ConfigKeyOrder = "behaviour.mentions.order"
    const EntryIdAttribute = "data-gob-entryid"

    const binding = GobConfigHelper.makeDatabinding(gobconfig)

    const txtMentions = $("#cmentions_mentions")
    GobConfigHelper.bindTextCollection(binding, txtMentions)

    const template = $("#cmention_template_mention")
    const mentionTable = $("#cmentions_mention_table")

    function addEntryToTable(entryId) {
        if (entryId === null || entryId === undefined || entryId.length === 0)
            throw new Error("Invalid entry id")

        const entry = $(template.html())
        entry.appendTo(mentionTable)
        entry.attr(EntryIdAttribute, entryId)

        const entryBinding = GobConfigHelper.makeDatabinding(gobconfig)
        entry.data("configbinding", entryBinding)

        const configKey = ConfigKeyData + "." + entryId

        const btnDelete = entry.find(".tmpl-delete-entry")
        btnDelete.on("click", event => {
            event.stopPropagation()
            {
                (async () => {
                    const result = await GobConfigHelper.showConfirmationDialog({ dialogText: "config.mentions.entry.delete.dialog" })
                    if (result !== 1) return

                    gobconfig.remove(configKey)
                    const order = gobconfig.get(ConfigKeyOrder)
                    _.remove(order, e => e === entryId)
                    gobconfig.set(ConfigKeyOrder, order)
                })()
            }
        })
        btnDelete.attr("disabled", gobconfig.get(ConfigKeyOrder).length <= 1)

        const txtTrigger = entry.find(".tmpl-trigger")
        txtTrigger.val(gobconfig.get(configKey + ".trigger").join(", "))
        txtTrigger.on("change", (event) => {
            let words = (event.target.value || "").split(",")
            words = words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
            gobconfig.set(configKey + ".trigger", words)
            txtTrigger.val(words.join(", "))
        })

        /* deactivated til multiple mentions are really needed
        function makeColorSelector(classId, configKey) {
            const selectorElement = entry.find(`.${classId}`)
            selectorElement.attr(GobConfigHelper.ConfigKeyAttribute, configKey)
            GobConfigHelper.makeColorSelector(selectorElement)
            selectorElement.spectrum("set", gobconfig.get(configKey))

            const resetButton = entry.find(`.${classId}-reset`)
            resetButton.on("click", event => {
                gobconfig.set(configKey, "#9358E4")
                selectorElement.spectrum("set", gobconfig.get(configKey));
            })
        }

        makeColorSelector("tmpl-color-forground", configKey + ".style.color")
        */

        const ckbPlaySound = entry.find(".tmpl-play-sound")
        ckbPlaySound.prop("checked", gobconfig.get(configKey + ".playSound"))
        ckbPlaySound.on("change", (event) => {
            gobconfig.set(configKey + ".playSound", event.target.checked)
            entry.find(".tmp-sound-options").toggle(event.target.checked)
        })
        entry.find(".tmp-sound-options").toggle(ckbPlaySound.prop("checked"))

        function isSoundFileValid(path) {
            const isValid = Gobchat.isString(path) && path.length > 0
            return isValid
        }

        const txtSoundPath = entry.find(".tmpl-sound-path")
        txtSoundPath.val(gobconfig.get(configKey + ".soundPath") || "")
        txtSoundPath.on("change", (event) => {
            const soundFile = event.target.value
            const isValid = isSoundFileValid(soundFile)
            btnCheckSound.attr("disabled", !isValid)
            gobconfig.set(configKey + ".soundPath", isValid ? soundFile : "")
        })

        const btnSoundSelector = entry.find(".tmpl-sound-selector")
        btnSoundSelector.on("click", (event) => {
            const fileSelector = document.createElement("input")
            fileSelector.type = "file"
            fileSelector.onchange = (event) => {
                const file = event.target.files[0];
                if (file)
                    txtSoundPath.val(`../sounds/${file.name}`)
                else
                    txtSoundPath.val("")
                txtSoundPath.change()
                checkPlayability(file.type || "")
            }
            fileSelector.click();
        })

        const btnCheckSound = entry.find(".tmpl-test-sound")
        btnCheckSound.attr("disabled", !isSoundFileValid(txtSoundPath.val()))
        btnCheckSound.on("click", (event) => {
            (async () => {
                const soundPath = gobconfig.get(configKey + ".soundPath")
                const audio = new Audio("../" + soundPath);
                audio.volume = gobconfig.get(configKey + ".volume")

                try {
                    await audio.play()
                } catch (e) {
                    console.error(e)
                    GobConfigHelper.showErrorDialog({ dialogText: "config.mentions.audio.test.error" });
                }
            })()
        })

        const sldSoundVolume = entry.find(".tmpl-sound-volume")
        const normalize = (value) => { return (value - 0) / (100 - 0) }
        const denormalize = (value) => { return value * (100 - 0) + 0 }
        sldSoundVolume.val(denormalize(gobconfig.get(configKey + ".volume")))
        sldSoundVolume.on("change", (event) => {
            gobconfig.set(configKey + ".volume", normalize(event.target.value))
        })

        function checkPlayability(format) {
            const audio = new Audio();
            const canPlay = audio.canPlayType(format)
            const result = canPlay === "probably" ? 2 : canPlay === "maybe" ? 1 : 0

            entry.find(".tmpl-canplay-0, .tmpl-canplay-1, .tmpl-canplay-2").hide()

            if (result === 0) entry.find(".tmpl-canplay-0").show()
            if (result === 1) entry.find(".tmpl-canplay-1").show()
            if (result === 2) entry.find(".tmpl-canplay-2").show()
        }

        const txtSoundRest = entry.find(".tmpl-sound-rest")
        txtSoundRest.val(gobconfig.get(configKey + ".soundInterval") / 1000)
        txtSoundRest.on("change", function (event) {
            gobconfig.set(configKey + ".soundInterval", (event.target.value || 0) * 1000)
        })
    }

    async function populateTable() {
        clearTableTable()
        const ids = gobconfig.get(ConfigKeyOrder)
        ids.forEach(id => addEntryToTable(id))
        await goblocale.updateElement(mentionTable)
        /* deactivated til multiple mentions are really needed
        mentionTable.sortable("refresh")
        mentionTable.accordion("refresh")
        */
    }

    function clearTableTable() {
        mentionTable.find("div[data-gob-entryid]").each(function (idx) {
            $(this).data("configbinding").clear()
        })
        mentionTable.empty()
    }

    function addNewEntry() {
        function generateId(ids) {
            let id = null
            do {
                id = Gobchat.generateId(6)
            } while (_.includes(ids, id))
            return id
        }

        const data = gobconfig.get(ConfigKeyData)
        const id = generateId(Object.keys(data))

        data[id] = {
            id: id,
            trigger: [],
            playSound: false,
            soundPath: null,
            volume: 1.0,
            /* deactivated til multiple mentions are really needed
            style: {
                "color": "#9358E4"
            }
            */
        }

        gobconfig.set(ConfigKeyData, data)

        const order = gobconfig.get(ConfigKeyOrder)
        order.push(id)
        gobconfig.set(ConfigKeyOrder, order)
    }

    /* deactivated til multiple mentions are really needed
    mentionTable.accordion({
        heightStyle: "content",
        header: "> div > h4",
    })
        .sortable({
            axis: "y",
            handle: "div",
            stop: function (event, ui) {
                let order = []
                mentionTable.find("div[data-gob-entryid]").each(function () {
                    order.push($(this).attr(EntryIdAttribute))
                })

                order = order.filter(e => e !== null && e !== undefined && e.length > 0)
                gobconfig.set(ConfigKeyOrder, order)
            }
        });
    */

    gobconfig.addProfileEventListener((event) => {
        (async () => {
            if (event.type === "active")
                await populateTable()
        })()
    })
    gobconfig.addPropertyEventListener(ConfigKeyOrder, (event) => {
        (async () => {
            if (event.isActive)
                await populateTable()
        })()
    })
    await populateTable()

    const btnAddMentionBox = $("#cmentions_add_new_mention1, #cmentions_add_new_mention2")
    btnAddMentionBox.on("click", (event) => addNewEntry())

    binding.initialize()

    const btnCopyProfile = $("#cmentions_copyprofile")
    const copyKeys = ["behaviour.mentions.data", "behaviour.mentions.order"]
    GobConfigHelper.makeCopyProfileButton(btnCopyProfile, { configKeys: copyKeys })
}());

//# sourceURL=config_mentions.js