'use strict'

var Gobchat = (function (Gobchat, undefined) {
    const MessageSegmentEnum = Gobchat.MessageSegmentEnum

    class MessageSoundPlayer {
        constructor(config) {
            this._config = config
            this._lastSoundPlayed = new Date()
        }

        checkForSound(message) {
            const data = this._config.get("behaviour.mentions.data.base")
            if (!data.playSound || data.volume <= 0 || !data.soundPath)
                return

            const time = new Date()
            if (time - this._lastSoundPlayed < data.soundInterval)
                return

            const hasMention = message.segments.some(segment => { return segment.segmentType === MessageSegmentEnum.MENTION })
            if (!hasMention)
                return

            this._lastSoundPlayed = time
            const audio = new Audio(data.soundPath)
            audio.volume = data.volume
            audio.play()
        }
    }

    Gobchat.MessageSoundPlayer = MessageSoundPlayer

    return Gobchat
}(Gobchat || {}));