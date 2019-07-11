# VideoMusicSyncer
Written for a specific video editing task but parts of the code used may be more generally useful

Potentially useful code:
 - For constructing Windows Movie Maker Files (for audio & video only or as a starting point for more): WmmBuilder
 - For constructing FFMpeg commands (perhaps with some limitations though I'm not sure): FFMpegCommandBuilder
 - For setting up timings within music, skipping easily between beats, fractions of beats, bars, and so on: Beats
 - For constructing FFMpeg commands to overlay transparent effects (eg. circular 'glow' effects with the right image files) onto a video, synchronized with music: VideoGlowOverlay
 - For speeding up and slowing down parts of a video to match timings in music: VideoMusicSync
 - For an example of how some of these can be used (almost all parts are eventually used, but indirectly): Controller