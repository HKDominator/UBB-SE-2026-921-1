function addSkill() {

    const container = document.getElementById("skills-container");

    const index = container.children.length;

    container.insertAdjacentHTML(
        "beforeend",
        `
        <div class="mb-2">
            <input name="Skills[${index}].Skill.Name"
                   class="form-control" />
        </div>
        `
    );
}

function addWorkExperience() {

    const container = document.getElementById("work-container");

    const index = container.children.length;

    container.insertAdjacentHTML(
        "beforeend",
        `
        <div class="card p-3 mb-3">

            <input name="WorkExperiences[${index}].Company"
                   class="form-control mb-2"
                   placeholder="Company" />

            <input name="WorkExperiences[${index}].JobTitle"
                   class="form-control mb-2"
                   placeholder="Job Title" />

            <textarea name="WorkExperiences[${index}].Description"
                      class="form-control"></textarea>

        </div>
        `
    );
}

function addProject() {

    const container = document.getElementById("projects-container");

    const index = container.children.length;

    container.insertAdjacentHTML(
        "beforeend",
        `
        <div class="card p-3 mb-3">

            <input name="Projects[${index}].Name"
                   class="form-control mb-2"
                   placeholder="Project Name" />

            <textarea name="Projects[${index}].Description"
                      class="form-control"></textarea>

        </div>
        `
    );
}