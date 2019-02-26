<!-- thumbnail: https://i.stack.imgur.com/iXnc9.png --> 

# Rodgort

## What is this?

A project created by the team in SOBotics ([Chat](https://chat.stackoverflow.com/rooms/111347/sobotics), [GitHub](https://github.com/SOBotics/)) designed to make managing burnination, synonym and merge requests easier on Stack Overflow. You can find the dashboard [here](https://rodgort.sobotics.org/), and the source code [here](https://gitlab.com/rjrudman/Rodgort).

In short, it provides an insight into:

- Tags which were burnt that have re-appeared. [All 'zombies' detected since the start of Rodgort](https://rodgort.sobotics.org/zombies?onlyAlive=false).
- Requests for which the tracked tags have zero questions, but have not been marked `status-completed`. [Example](https://rodgort.sobotics.org/requests?status=none&hasQuestions=no).
- Remove the manual process of tracking burnination requests. [A filterable view of all requests found](https://rodgort.sobotics.org/requests).
- Requests which were completed, but there are still questions found in the burnt tags. [Example](https://rodgort.sobotics.org/requests?status=status-completed&hasQuestions=yes).
- Track the progress of burns, and review past burns. 
    - [Current burns](https://rodgort.sobotics.org/progress)
    - [The burn of 'ratio'](https://rodgort.sobotics.org/progress?metaQuestionId=277705)
    - [All burns tracked by Rodgort](https://rodgort.sobotics.org/tracked-burns)
- Manages [Burnaki](https://stackapps.com/questions/7027/burnaki-tracking-progress-and-helping-burnination-efforts-on-stack-exchange) (currently running under the account [Gemmy](https://chat.stackoverflow.com/users/8300708/gemmy)).   
    When a request is tagged `featured`, Rodgort will create an observation room ([an example](https://chat.stackoverflow.com/transcript/188947)) and instruct Burnaki to track the tag being discussed.  
    Rodgort also has a command (`@Rodgort untrack [tag]`) to which will automatically clean up any tracking Rodgort has asked Burnaki to do for that tag.

## How?

- Rodgort watches the live websocket of https://meta.stackoverflow.com for active questions, and queries all questions with the tags `burninate-request`, `synonym-request`, `retag-request` and `tag-disambiguation` daily.  
    When a post is processed, Rodgort will save a snapshot of the current score and view count of the request.  
    From the body of the request, Rodgort will extract all tags mentioned in the format of `[tag:tag-name]` and associate the tag with the request. If Rodgort deems a tag to be 'obviously' related (one heuristic is that there's only one tag found in the body, and it matches a tag found in the title exactly), it'll automatically start tracking the question count of the tag.  
    For those tags which it *can't* automatically guess, the tags will be marked in the system as 'requires tracking approval'. Privileged users of Rodgort will then be asked to manually mark whether or not the tag found is relevant to the request.

- Hourly, Rodgort will grab the question count of all tags which are being tracked. Part of this process is also identifying tag synonyms. Unfortunately, the API for tags which are synonyms is [not consistent](https://meta.stackexchange.com/questions/320542/api-tags-tag-list-info-returning-incorrect-count-for-synonyms) and thus may cause Rodgort to not recognize the tag being synonymised for a few hours.

- When a request is tagged `featured`, Rodgort will orchestrate Burnaki to setup an observation room, and instruct Burnaki to start tracking the tag. The burn will also now appear on Rodgort's [progress page](https://rodgort.sobotics.org/progress).

- When Burnaki reports a post, Rodgort will use the API to detect the changes made to the question: closures, re-opens, tag removal, tag addition, deletion and undeletion. In addition, Rodgort will also query the tag every day to identify changes Burnaki failed to post in the room.

## What's left to do? 

- We're currently tracking a *heap* of information regarding tags. However, not everything is easily accessible. I plan to create dashboards to:
    - Suggest requests which might be worth declining, based on question count, request score, request views and answers to the request
    - Suggest candidates for the next burnination from the pool of requests
- Due to the enormous amount of burnination requests (currently at 2466 - of which 1,131 have been marked `status-completed` or `status-declined`), there's still a large amount of triage to do. Currently, 630 requests need triaging. Triaging requests will instruct Rodgort to being tracking the question counts of tags - without which, we're unable to gain proper insight into the status of burns and tags.