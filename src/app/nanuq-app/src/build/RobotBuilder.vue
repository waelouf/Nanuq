<template >
    <div class="content">
      <button class="add-to-cart" @click="addToCart()">Add to Cart</button>
        <div class="top-row">
          <div class="top part" :class="{'sale-border': selectedRobot.head.onSale}">
            <div class="robot-name">
                {{ selectedRobot.head.title }}
                <span v-if="selectedRobot.head.onSale" class="sale">
                  Sale!
                </span>
            </div>
            <img :src="selectedRobot.head.src" title="head" alt=""/>
            <button @click="selectPreviousHead()" class="prev-selector">&#9668;</button>
            <button @click="selectNextHead()" class="next-selector">&#9658;</button>
          </div>
        </div>
        <div class="middle-row">
          <div class="left part">
            <img :src="selectedRobot.leftArm.src" title="left arm" alt=""/>
            <button @click="selectPreviousLeftArm()" class="prev-selector">&#9650;</button>
            <button @click="selectNextLeftArm()" class="next-selector">&#9660;</button>
          </div>
          <div class="center part">
            <img :src="selectedRobot.torso.src" title="torso" alt=""/>
            <button @click="selectPreviousTorso()" class="prev-selector">&#9668;</button>
            <button @click="selectNextTorso()" class="next-selector">&#9658;</button>
          </div>
          <div class="right part">
            <img :src="selectedRobot.rightArm.src" title="right arm" alt=""/>
            <button @click="selectPreviousRightArm()" class="prev-selector">&#9650;</button>
            <button @click="selectNextRightArm()" class="next-selector">&#9660;</button>
          </div>
        </div>
        <div class="bottom-row">
          <div class="bottom part">
            <img :src="selectedRobot.bottom.src" title="bottom" alt=""/>
            <button @click="selectPreviousBottom()" class="prev-selector">&#9668;</button>
            <button @click="selectNextBottom()" class="next-selector">&#9658;</button>
          </div>
        </div>
        <div>
          <h1>Cart</h1>
          <table>
            <thead>
              <tr>
                <th>Robot</th>
                <th class="cost">Cost</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(robot, index) in cart" :key="index">
                <td>
                  {{ robot.head.title }}
                </td>
                <td class="cost">
                  {{  robot.cost  }}
                </td>
              </tr>
            </tbody>
          </table>
          </div>
      </div>
</template>
<script>
import availableParts from '../data/parts';
import createdHookMixin from './created-hook-mixin';

function getPreviousValidIndex(index, length) {
  const deprecatedIndex = index - 1;
  return deprecatedIndex < 0 ? length - 1 : deprecatedIndex;
}

function getNextValidIndex(index, length) {
  const incrementedIndex = index + 1;
  return incrementedIndex > length - 1 ? 0 : incrementedIndex;
}

export default {
  name: 'RobotBuilder',
  mixins: [createdHookMixin],
  data() {
    return {
      availableParts,
      selectedHeadIndex: 0,
      selectedLeftArmIndex: 0,
      selectedRightArmIndex: 0,
      selectedTorsoIndex: 0,
      selectedBottomIndex: 0,
      cart: [],
    };
  },
  computed: {
    selectedRobot() {
      return {
        head: availableParts.heads[this.selectedHeadIndex],
        leftArm: availableParts.arms[this.selectedLeftArmIndex],
        rightArm: availableParts.arms[this.selectedRightArmIndex],
        torso: availableParts.torsos[this.selectedTorsoIndex],
        bottom: availableParts.bases[this.selectedBottomIndex],
      };
    },
    headerBorderStyle() {
      return {
        border: this.selectedRobot.head.onSale ? '3px solid red' :
          '3px solid #aaa',
      };
    },
  },
  methods: {
    selectNextHead() {
      this.selectedHeadIndex =
        getNextValidIndex(this.selectedHeadIndex, this.availableParts.heads.length);
    },
    selectPreviousHead() {
      this.selectedHeadIndex =
        getPreviousValidIndex(this.selectedHeadIndex, this.availableParts.heads.length);
    },
    selectNextLeftArm() {
      this.selectedLeftArmIndex =
        getNextValidIndex(this.selectedLeftArmIndex, this.availableParts.arms.length);
    },
    selectPreviousLeftArm() {
      this.selectedLeftArmIndex =
        getPreviousValidIndex(this.selectedLeftArmIndex, this.availableParts.arms.length);
    },
    selectNextRightArm() {
      this.selectedRightArmIndex =
        getNextValidIndex(this.selectedRightArmIndex, this.availableParts.arms.length);
    },
    selectPreviousRightArm() {
      this.selectedRightArmIndex =
        getPreviousValidIndex(this.selectedRightArmIndex, this.availableParts.arms.length);
    },
    selectNextTorso() {
      this.selectedTorsoIndex =
        getNextValidIndex(this.selectedTorsoIndex, this.availableParts.torsos.length);
    },
    selectPreviousTorso() {
      this.selectedTorsoIndex =
        getPreviousValidIndex(this.selectedTorsoIndex, this.availableParts.torsos.length);
    },
    selectNextBottom() {
      this.selectedBottomIndex =
        getNextValidIndex(this.selectedBottomIndex, this.availableParts.bases.length);
    },
    selectPreviousBottom() {
      this.selectedBottomIndex =
        getPreviousValidIndex(this.selectedBottomIndex, this.availableParts.bases.length);
    },
    addToCart() {
      const robot = this.selectedRobot;
      const cost = robot.head.cost + robot.leftArm.cost + robot.rightArm.cost
      + robot.torso.cost + robot.bottom.cost;
      this.cart.push({ ...robot, cost });
    },
  },
};
</script>
<style lang="scss" scoped>
.part {
    position: relative;
    width:165px;
    height:165px;
    border: 3px solid #aaa;
  }
  .part {
    img {
      width:165px;
    }
  }
  .top-row {
    display:flex;
    justify-content: space-around;
  }
  .middle-row {
    display:flex;
    justify-content: center;
  }
  .bottom-row {
    display:flex;
    justify-content: space-around;
    border-top: none;
  }
  .head {
    border-bottom: none;
  }
  .left {
    border-right: none;
  }
  .right {
    border-left: none;
  }
  .left img {
    transform: rotate(-90deg);
  }
  .right img {
    transform: rotate(90deg);
  }
  .bottom {
    border-top: none;
  }
  .prev-selector {
    position: absolute;
    z-index:1;
    top: -3px;
    left: -28px;
    width: 25px;
    height: 171px;
  }
  .next-selector {
    position: absolute;
    z-index:1;
    top: -3px;
    right: -28px;
    width: 25px;
    height: 171px;
  }
  .center .prev-selector, .center .next-selector {
    opacity:0.8;
  }
  .left .prev-selector {
    top: -28px;
    left: -3px;
    width: 144px;
    height: 25px;
  }
  .left .next-selector {
    top: auto;
    bottom: -28px;
    left: -3px;
    width: 144px;
    height: 25px;
  }
  .right .prev-selector {
    top: -28px;
    left: 24px;
    width: 144px;
    height: 25px;
  }
  .right .next-selector {
    top: auto;
    bottom: -28px;
    left: 24px;
    width: 144px;
    height: 25px;
  }
  .right .next-selector {
    right: -3px;
  }
  .robot-name {
    position: absolute;
    top: -25px;
    text-align: center;
    width: 100%
  }
  .sale{
    color:red;
  }
  .content{
    position: relative;
  }
  .add-to-cart{
    position: absolute;
    right: 30px;
    width: 220px;
    padding: 3px;
    font-size: 16px;
  }
  td, th {
    text-align: left;
    padding: 5px;
    padding-right: 20px;
  }
  .cost {
    text-align: right;
  }
  .sale-border{
    border: 3px solid red;
  }
</style>
