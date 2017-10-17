var gulp = require("gulp");
var zip = require('gulp-zip');


gulp.task("zip-blender", function () {
    return gulp.src('Blender/src/**')
        .pipe(zip('Blender2Babylon-5.4.zip'))
        .pipe(gulp.dest('Blender'));
});