var gulp = require("gulp");
var zip = require('gulp-zip');


gulp.task("zip-blender", function () {
    return gulp.src('Blender/src/**')
        .pipe(zip('Blender2Babylon-6.0.zip'))
        .pipe(gulp.dest('Blender'));
});