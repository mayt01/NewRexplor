let voiceIndex = 1;
showVoices(voiceIndex);

function plusVoices(n) {
    showVoices(voiceIndex += n);
}

function currentVoice(n) {
    showVoices(voiceIndex = n);
}

function showVoices(n) {
    let i;
    let voices = document.getElementsByClassName("myVoices");
    let dotVoices = document.getElementsByClassName("dotVoice");
    if (n > voices.length) { voiceIndex = 1 }
    if (n < 1) { voiceIndex = voices.length }
    for (i = 0; i < voices.length; i++) {
        voices[i].style.display = "none";
    }
    for (i = 0; i < dotVoices.length; i++) {
        dotVoices[i].className = dotVoices[i].className.replace(" active", "");
    }
    voices[voiceIndex - 1].style.display = "flex";
    dotVoices[voiceIndex - 1].className += " active";
}