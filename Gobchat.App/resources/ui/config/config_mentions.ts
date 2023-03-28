/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

import * as Databinding from "/module/Databinding"
import * as Dialog from "/module/Dialog"
import * as Utility from "/module/CommonUtility"
import * as Components from "/module/Components"

const binding = new Databinding.BindingContext(gobConfig)

const txtTrigegrWords = $("#cp-mentions_triggerwords")
const chkCheckUserMsgForMentions = $("#cp-mentions_mentions-on-user")
const chkPlaySound = $("#cp-mentions_audio-play")
const txtAudioFilePath = $("#cp-mentions_audio-path")
const btnOpenAudioFileDialog = $("#cp-mentions_audio-path_select")
const btnPlayAudio = $("#cp-mentions_audio-test")
const sliderAudioVolume = $("#cp-mentions_audio-volume")
const txtAudioReplayInterval = $("#cp-mentions_audio-replay-interval")
const chkIgnoreRangeFilter = $("#cp-mentions_ignore-range-filter")

const iconCanPlay = $("")
const iconMaybePlay = $("")
const iconProbablyPlay = $("")

Databinding.bindElement(binding, txtTrigegrWords, {
    elementToConfig: (element) => {
        let words = element.val().split(",")
        words = words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
        element.val(words.join(", "))
        return words
    },
    configToElement: (element, storedValue) => {
        element.val((storedValue || []).join(", "))
    }
})

Databinding.bindCheckbox(binding, chkCheckUserMsgForMentions)
Databinding.bindCheckbox(binding, chkPlaySound)


function isSoundFileValid(path) {
    return Utility.isNonEmptyString(path)
}

function showPlayabilityIcon(format) {
    const audio = new Audio();
    const canPlay = audio.canPlayType(format)
    const result = canPlay === "probably" ? 2 : canPlay === "maybe" ? 1 : 0

    iconCanPlay.hide()
    iconMaybePlay.hide()
    iconProbablyPlay.hide()

    if (result === 0) iconCanPlay.show()
    if (result === 1) iconMaybePlay.show()
    if (result === 2) iconProbablyPlay.show()
}

Databinding.bindElement(binding, txtAudioFilePath, {
    elementToConfig: (element) => {
        const newSoundFile = element.val()
        const isValid = isSoundFileValid(newSoundFile)
        btnPlayAudio.prop("disabled", !isValid)
        return newSoundFile
    },
    configToElement: (element, storedValue) => {
        const isValid = isSoundFileValid(storedValue)
        btnPlayAudio.prop("disabled", !isValid)
        element.val(storedValue)
    }
})

btnOpenAudioFileDialog.on("click", function () {
    const fileSelector = document.createElement("input")
    fileSelector.type = "file"
    fileSelector.onchange = (event) => {
        const inputElement = event.target as HTMLInputElement
        const file = inputElement.files ? inputElement.files[0] : null

        if (file)
            txtAudioFilePath.val(`../sounds/${file.name}`)
        else
            txtAudioFilePath.val("")

        txtAudioFilePath.change()
        showPlayabilityIcon(file && file.type || "")
    }
    fileSelector.click();
})

btnPlayAudio.on("click", async function () {
    const soundPath = gobConfig.get(Databinding.getConfigKey(txtAudioFilePath))
    const audio = new Audio("../" + soundPath);
    audio.volume = gobConfig.get(Databinding.getConfigKey(sliderAudioVolume))

    try {
        await audio.play()
    } catch (e) {
        console.error(e)
        Dialog.showErrorDialog({ dialogText: "config.mentions.audio.test.error" });
    }
})

Databinding.bindElement(binding, sliderAudioVolume, {
    elementToConfig: (element) => {
        return (parseFloat(element.val()) || 0) / 100
    },
    configToElement: (element, storedValue) => {
        element.val(storedValue * 100)
    }
})

Databinding.bindElement(binding, txtAudioReplayInterval, {
    elementToConfig: (element) => {
        return (parseFloat(element.val()) || 0) * 1000
    },
    configToElement: (element, storedValue) => {
        element.val(storedValue / 1000)
    }
})

Databinding.bindCheckbox(binding, chkIgnoreRangeFilter)

binding.loadBindings()

Components.makeCopyProfileButton($("#cp-mentions_copyprofile"), { configKeys: ["behaviour.mentions", "behaviour.rangefilter.ignoreMention"] })

//# sourceURL=config_mentions.js