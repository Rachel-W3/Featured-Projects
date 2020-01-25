"use strict";
window.onload = init;

function init(){
    /* Activate button with Enter key */
    document.querySelector("#searchterm").addEventListener("keyup", event =>{
        if(event.key !== "Enter") return; 
        document.querySelector("#search").click(); 
        event.preventDefault();
    });
    document.querySelector("#search").onclick = getData;
    if(localStorage.getItem('lastKey') !==null){
        document.getElementById("searchterm").value = localStorage.getItem('lastKey');
    }else{

        document.getElementById("searchterm").placeholder= "Enter a game title";
    }
}

let term = ""; // we declared `term` out here because we will need it later
function getData(){

    // 1 - main entry point to web service
    const SERVICE_URL = "https://api.rawg.io/api/games?search=";
    
    // No API Key required!
    
    // 2 - build up our URL string
    let url = SERVICE_URL;
    
    // 3 - parse the user entered term we wish to search
    term = document.querySelector("#searchterm").value;


    //local storage
    // Put the object into storage
    localStorage.setItem('lastKey', term);
    
    // Retrieve the object from storage
    document.getElementById("searchterm").placeholder = localStorage.getItem('lastKey');
    console.log(localStorage.getItem('lastKey'));

    // get rid of any leading and trailing spaces
    term = term.trim();
    // encode spaces and special characters
    term = encodeURIComponent(term);
    url += term;
    
    // 4 - update the UI
    // document.querySelector("#debug").innerHTML = `<b>Querying web service with:</b> <a href="${url}" target="_blank">${url}</a>`;
    
    // 5- call the web service, and prepare to download the file
    $.ajax({
        dataType: "json",
        url: url,
        data: null,
        success: jsonLoaded
    });
}

$(document).ready(function () {
    $("#search").click(function() {
        $('html, body').animate({
            scrollTop: $("#content").offset().top
        }, 2000);
    });
});

let minRating = 0;
let loadedResults = [];

function jsonLoaded(obj) 
{
    console.log(obj);
    loadedResults = obj.results;
    
    filterResults();
}

function getMinRating() {
    let ratingFilter = document.getElementsByClassName("rating-filter");

    for(let i = 0; i < ratingFilter.length; i++) {
        if(ratingFilter[i].checked) {
            let minRating = parseFloat(ratingFilter[i].value, 10);

            if(isNaN(minRating)) {
                minRating = 0;
            }
            
            return minRating;
        }
    }

    /* If nothing is checked, check 'all' */
    ratingFilter[0].checked = true;
    return 5;
}

function getCheckedCheckboxes() {
    let checkboxes = document.getElementsByClassName("platform-filter");
    let checked = [];

    for(let i = 0; i < checkboxes.length; i++) {
        if(checkboxes[i].checked) {
            checked.push(checkboxes[i]);
        }
    }

    return checked;
}

function filterResults() {
    minRating = getMinRating();
    let checkedBoxes = getCheckedCheckboxes();
    let filteredResults = [];
    let platforms = [];
    
    for(let i = 0; i < loadedResults.length; i++) {
        let result_passed = false;

        // filtering by rating
        if(loadedResults[i].rating < minRating) {
            continue;
        }
        // -----------------------------------------------------------------------------------

        // filtering by platform/console
        if(checkedBoxes.length == 0) {
            filteredResults.push(loadedResults[i]); // if no boxes are checked, every result gets added to the list
            continue;
        }
        for(let j = 0; j < checkedBoxes.length; j++) {
            for(let k = 0; k < loadedResults[i].platforms.length; k++) {
                if(loadedResults[i].platforms[k].platform.name == checkedBoxes[j].value) {
                    result_passed = true;
                }
            }
        }
        // -----------------------------------------------------------------------------------

        // Push results that passed the filters into new array
        if (result_passed) {
            console.log(loadedResults[i].platforms);
            filteredResults.push(loadedResults[i]);
        }
        // -----------------------------------------------------------------------------------
    }

    // Sort filtered results by highest ratings if checkbox is checked
    let sort_checkbox = document.getElementById("sort-filter");
    if(sort_checkbox.checked) {
        let orderedResults = filteredResults.sort(compareRatings);
        draw(orderedResults);
        return;
    }

    draw(filteredResults);
}

function compareRatings(first,second) {
    if (first.rating == second.rating) {
        return 0;
    }
    if (first.rating < second.rating) {
        return 1;
    }
    return -1;
}

function draw(results){
    let line = `<div id='flex-container'>`;

    if (results.length == 0) {
        line += `<p>No results were found. Try changing the filter specifications.</p>`
        line += `</div>`; /* closing flex-container div */
        document.querySelector("#content").innerHTML = line;
        return;
    }

    for (let i = 0; i < results.length; i++) {

        let result = results[i];
        let name = result.name;
        let imgURL = result.background_image;
        let rating = result.rating;
        let platformCount = result.platforms;
        let platformString = `<p>Platforms: `;
        let otherString = ``;

        if(result.platforms.length > 3) {
            platformCount = 3; // Cap the number of platforms to display
            otherString = `Other...`;
        }

        for(let j = 0; j < platformCount; j++) {
            platformString += `${result.platforms[j].platform.name}, `;
        }

        platformString += otherString;

        if(imgURL == null){
            imgURL = 'http://placehold.it/250x250';
        }

        line += `<div class='result'>`;
        line += `<img class='preview-image' src='${imgURL}' title= '${result.id}' />`;
        line += `<p class='game-name'>${name}</p>`;
        line += `<p>Ratings: ${rating}</p>`;
        line += platformString;
        line += `</div>`;

    }

    line += `</div>`; /* closing flex-container div */
    
    document.querySelector("#content").innerHTML = line;
}